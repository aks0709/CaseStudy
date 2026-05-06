# Frontend Flow — marketplace-ui (Angular 21)

## Case Study Overview

`marketplace-ui` is the Angular 21 frontend for the MultiClientPlatform marketplace. It provides two distinct user experiences on the same app — a **Customer** who browses merchants, manages a cart, places orders, and pays; and a **Merchant** who manages their store profile, lists products, and views incoming orders. The app is a single-page application (SPA) with lazy-loaded feature modules, JWT-based session management, and role-based routing.

**Tech Stack:**
- Angular 21 (NgModule-based, not standalone components)
- RxJS for HTTP and async operations
- Angular Reactive Forms for all form handling
- JWT stored in `localStorage`
- `provideHttpClient` with functional interceptor
- `provideZoneChangeDetection` for change detection

---

## Project Structure

``` 
src/app/
  auth/           — Login + Register page
  catalog/        — Browse merchants and their products (Customer)
  cart/           — View and manage cart (Customer)
  order/          — Order history (Customer)
  payment/        — Payment flow (Customer)
  merchant/       — Dashboard + Orders Received (Merchant)
  shared/
    components/   — Navbar
    guards/       — Route protection (auth, customer, merchant)
    interceptors/ — JWT interceptor
    models/       — TypeScript interfaces for all API shapes
    services/     — One service per feature (AuthService, CartService, etc.)
    validation.ts — Reusable form validators
  app-module.ts
  app-routing-module.ts
```

---

## App Bootstrap

**`main.ts`** calls `platformBrowser().bootstrapModule(AppModule)`.

`platformBrowser` is the correct bootstrap API in Angular 21 when `@angular/platform-browser-dynamic` is not installed. It bootstraps the app using the browser platform directly.

**`app-module.ts`** providers:
```typescript
providers: [
  provideZoneChangeDetection({ eventCoalescing: true }),
  provideHttpClient(withInterceptors([jwtInterceptor]))
]
```

**Why `provideZoneChangeDetection` is required:**
In Angular 21, when bootstrapping via `platformBrowser().bootstrapModule()`, Angular does NOT automatically wire zone.js to its change detection scheduler. Without `provideZoneChangeDetection()`, Angular defaults to `NoopNgZone` — zone.js patches browser APIs (XHR, setTimeout, etc.) but Angular never schedules a change detection cycle after async operations complete. This means HTTP responses would update component state but the UI would not re-render until the next user interaction. Explicitly providing `provideZoneChangeDetection({ eventCoalescing: true })` wires zone.js properly so Angular runs change detection after every async event.

`eventCoalescing: true` batches multiple events that fire in the same microtask into a single change detection run — a performance optimization.

**Why functional interceptor:**
`provideHttpClient` requires interceptors as `HttpInterceptorFn` (functional style) passed via `withInterceptors([...])`. The old class-based `HTTP_INTERCEPTORS` token approach does not work with `provideHttpClient`.

---

## Routing

**`app-routing-module.ts`** defines top-level routes, all lazy-loaded:

```
/           → redirects to /auth
/auth       → AuthModule     (login/register)
/catalog    → CatalogModule  (browse merchants + products)
/cart       → CartModule     (cart management)
/order      → OrderModule    (order history)
/payment    → PaymentModule  (payment flow)
/merchant   → MerchantModule (merchant dashboard + orders)
**          → redirects to /auth
```

**Lazy loading** means each module's JavaScript bundle is only downloaded when the user first navigates to that route. This keeps the initial bundle small.

Each feature module defines its own child routes. For example, `PaymentModule` maps `/payment/:orderId` to `PaymentComponent`.

---

## Models (TypeScript Interfaces)

All API request/response shapes are defined in `shared/models/models.ts`. These are pure TypeScript interfaces — no classes, no decorators. They mirror the backend DTOs exactly.

Key interfaces:
- `AuthResponse` — `{ token, fullName, email, role }`
- `MerchantResponse` — `{ id, userId, businessName, description, createdAt }`
- `ProductResponse` — `{ id, merchantId, name, description, price, createdAt }`
- `CartResponse` — `{ items: CartItemResponse[], grandTotal }`
- `CartItemResponse` — `{ id, productId, productName, unitPrice, quantity, lineTotal }`
- `OrderResponse` — `{ id, userId, totalAmount, status, placedAt, items: OrderItemResponse[] }`
- `InitiatePaymentResponse` — `{ paymentId, orderId, status, paymentUrl, initiatedAt }`
- `PaymentStatusResponse` — `{ paymentId, orderId, paymentStatus, orderStatus, initiatedAt, completedAt }`

---

## JWT Interceptor

**`shared/interceptors/jwt.interceptor.ts`**

```typescript
export const jwtInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(AuthService).getToken();
  if (token) {
    return next(request.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    }));
  }
  return next(request);
};
```

**Concept: HttpInterceptorFn**
A functional interceptor is a plain function (not a class) that intercepts every outgoing HTTP request. It uses Angular's `inject()` to access services — this works because the function runs inside Angular's injection context.

Every HTTP request made by any service automatically gets the `Authorization: Bearer <token>` header attached if a token exists in localStorage. This means services never manually set auth headers — the interceptor handles it transparently.

Public endpoints (like `GET /api/merchant`) still work because the backend's `[AllowAnonymous]` endpoints ignore the header even if it's present.

---

## AuthService

**`shared/services/auth.service.ts`**

Manages the session entirely through `localStorage`. After a successful login or register, `storeSession()` saves:
- `token` — the JWT string
- `role` — "Customer" or "Merchant"
- `fullName` — display name
- `email`

Key methods:
- `login(request)` — POST to `/api/auth/login`, pipes through `tap(storeSession)`
- `register(request)` — POST to `/api/auth/register`, pipes through `tap(storeSession)`
- `logout()` — removes all four localStorage keys
- `isLoggedIn()` — `!!getToken()`
- `isCustomer()` / `isMerchant()` — checks role from localStorage
- `getToken()` — used by the JWT interceptor

**Concept: RxJS `tap`**
`tap` is a side-effect operator. It runs a function on each emitted value without transforming it. Here it's used to save the session data as a side effect of the HTTP response, while still passing the `AuthResponse` through to the subscriber.

---

## Route Guards

Three guards protect routes:

**`auth.guard.ts`** — redirects to `/auth` if not logged in. Applied to all protected routes.

**`customer.guard.ts`** — redirects if role is not "Customer". Applied to `/catalog`, `/cart`, `/order`, `/payment`.

**`merchant.guard.ts`** — redirects if role is not "Merchant". Applied to `/merchant`.

Guards read from `AuthService` (which reads from localStorage) to make their decisions synchronously.

---

## Navbar Component

**`shared/components/navbar.component.html`**

The navbar is declared in `AppModule` and rendered in `app.html` above `<router-outlet>`. It uses `*ngIf="authService.isLoggedIn()"` to hide itself on the login page.

Role-based link visibility:
```html
<ng-container *ngIf="authService.isCustomer()">
  <!-- Browse, Cart, Orders links -->
</ng-container>
<ng-container *ngIf="authService.isMerchant()">
  <!-- My Products, Orders Received links -->
</ng-container>
```

This is purely a UI concern — the backend enforces the actual access control. The navbar just avoids showing irrelevant links.

The logout button calls `authService.logout()` then navigates to `/auth`.

---

## Feature 1 — Auth (Login / Register)

**Route:** `/auth`
**Component:** `AuthComponent`
**Template:** `auth.component.html`

### What it does
Single page with a tab toggle between Login and Register. Uses Angular Reactive Forms with custom validators from `Validation`.

### Reactive Forms
`loginForm` and `registerForm` are `FormGroup` instances built with `FormBuilder`. Each field has validators. `getError(form, field)` calls `Validation.getError()` to return a human-readable error string for display under each input.

### Login Flow
```
User fills email + password → clicks Login
  → loginForm.invalid? → markAllAsTouched() (shows all errors)
  → authService.login(loginForm.value)
      → POST /api/auth/login
      → tap: storeSession() → localStorage.setItem(token, role, fullName, email)
  → subscribe next:
      → redirectAfterLogin(response.role)
          role === 'Merchant' → router.navigate(['/merchant/products'])
          role === 'Customer' → router.navigate(['/catalog'])
  → subscribe error:
      → errorMessage = err.error || 'Login failed...'
```

### Register Flow
Same as login but with `registerForm` which includes `fullName` and a role selector (Customer / Merchant buttons). After successful register, the backend returns a JWT immediately — the user is logged in without a separate login step.

---

## Feature 2 — Catalog (Browse Merchants + Products)

**Route:** `/catalog`
**Component:** `CatalogComponent`
**Guard:** Customer only

### What it does
Lists all merchants. Clicking a merchant expands it and lazily loads their products. Customer can add any product to their cart.

### State
```typescript
merchants: MerchantResponse[]
productsByMerchant: { [merchantId: number]: ProductResponse[] }
expandedMerchantId: number | null
addingProductId: number | null
successMessage: string
errorMessage: string
```

### ngOnInit
Calls `loadMerchants()` → `merchantService.getAll()` → `GET /api/merchant`. No auth token needed (public endpoint).

### Merchant Expansion (Lazy Product Loading)
```
User clicks merchant header
  → toggleMerchant(merchantId)
      → if already expanded → collapse (set expandedMerchantId = null)
      → else → expandedMerchantId = merchantId
          → productsByMerchant[merchantId] already loaded? → skip
          → else → productService.getByMerchant(merchantId)
              → GET /api/merchant/{merchantId}/products
              → next: productsByMerchant = { ...productsByMerchant, [merchantId]: products }
```

**Why spread operator for productsByMerchant update:**
`this.productsByMerchant[merchantId] = products` mutates the existing object reference. Angular's change detection (specifically `*ngIf` and `*ngFor` on object properties) may not detect this mutation. Using the spread operator `{ ...productsByMerchant, [merchantId]: products }` creates a new object reference, which Angular's change detection reliably picks up.

### Add to Cart
```
User clicks "+ Add to Cart" on a product
  → addToCart(product)
      → addingProductId = product.id  (disables button, shows "Adding...")
      → cartService.addItem({ productId: product.id, quantity: 1 })
          → POST /api/cart  [JWT auto-attached by interceptor]
      → next:
          → addingProductId = null
          → successMessage = '"ProductName" added to cart!'
          → timer(3000).subscribe(() => successMessage = '')
      → error:
          → addingProductId = null
          → errorMessage = err.error || 'Failed to add to cart.'
```

**Why `timer(3000)` instead of `setTimeout`:**
`setTimeout` is a plain browser API. When called inside an Angular zone, it works fine, but using RxJS `timer()` is idiomatic in Angular — it returns an Observable that integrates naturally with zone.js and Angular's change detection scheduler. `timer(3000)` emits once after 3 seconds, and the subscribe callback clears the success message.

---

## Feature 3 — Cart

**Route:** `/cart`
**Component:** `CartComponent`
**Guard:** Customer only

### What it does
Displays all cart items with quantities, unit prices, line totals, and a grand total. Allows quantity updates, item removal, and checkout.

### ngOnInit
Calls `loadCart()` → `cartService.getCart()` → `GET /api/cart` [JWT required].

The backend computes `lineTotal` and `grandTotal` — the frontend just displays them.

### Update Quantity
```
User clicks + or − button
  → updateQuantity(cartItemId, newQuantity)
      → if quantity < 1 → return (prevent zero/negative)
      → cartService.updateItem(cartItemId, { quantity })
          → PUT /api/cart/{cartItemId}
      → next: loadCart()  (reload to get fresh totals)
```

### Remove Item
```
User clicks Remove
  → removeItem(cartItemId)
      → cartService.removeItem(cartItemId)
          → DELETE /api/cart/{cartItemId}
      → next: loadCart()
```

### Checkout
```
User clicks "Proceed to Checkout"
  → checkout()
      → checkingOut = true
      → orderService.checkout()
          → POST /api/order/checkout  [JWT required]
          → backend: reads cart, creates Order + OrderItems, clears cart
      → next: (order: OrderResponse)
          → checkingOut = false
          → router.navigate(['/payment', order.id])
      → error:
          → checkingOut = false
          → errorMessage = err.error || 'Checkout failed.'
```

After checkout, the user is immediately navigated to the payment page with the new order's ID in the URL.

---

## Feature 4 — Payment

**Route:** `/payment/:orderId`
**Component:** `PaymentComponent`
**Guard:** Customer only

### What it does
Two-phase payment simulation:
1. On load — initiates payment, shows the dummy gateway URL and payment metadata
2. On button click — completes payment, shows success status

### ngOnInit
```
orderId = Number(route.snapshot.paramMap.get('orderId'))
initiatePayment()
  → paymentService.initiate(orderId)
      → POST /api/payment/initiate/{orderId}
  → next: payment = data  (stores InitiatePaymentResponse)
```

### Complete Payment
```
User clicks "Complete Payment"
  → completePayment()
      → completing = true
      → paymentService.complete(payment.paymentId)
          → POST /api/payment/complete/{paymentId}
          → backend: marks Payment=Completed, Order=Paid
      → next: (data: PaymentStatusResponse)
          → status = data
          → completing = false
```

When `status` is set, the template switches from showing the payment card to showing the success card (`*ngIf="status"`).

### Navigate to Orders
```
User clicks "View My Orders"
  → goToOrders()
      → router.navigate(['/order/history'])
```

---

## Feature 5 — Order History

**Route:** `/order/history`
**Component:** `OrderHistoryComponent`
**Guard:** Customer only

### What it does
Lists all orders for the logged-in customer. Each order shows its items, total, status badge (Pending/Paid), and a "Complete Payment" button for Pending orders.

### ngOnInit
```
loadOrders()
  → orderService.getMyOrders()
      → GET /api/order  [JWT required]
  → next: orders = data
```

### Status Badge
The template uses `[class.paid]` and `[class.pending]` class bindings on the badge element, driven by `order.status`. No logic in the component — purely template-driven styling.

### Complete Payment Button
Only shown for Pending orders via `*ngIf="order.status === 'Pending'"`. Clicking it calls `goToPayment(order.id)` which navigates to `/payment/{orderId}`. The payment component then re-initiates (or reuses the existing pending payment) automatically on load.

---

## Feature 6 — Merchant Dashboard

**Route:** `/merchant/products`
**Component:** `MerchantDashboardComponent`
**Guard:** Merchant only

### What it does
Two states:
1. No profile yet → shows "Set Up Your Store" form
2. Profile exists → shows store banner, product form (add/edit), and product list

### ngOnInit
```
loadProfile()
  → merchantService.getMyProfile()
      → GET /api/merchant/my-profile  [JWT required, Role=Merchant]
  → next: profile = data → loadProducts()
  → error 404: profile stays null → setup form is shown
  → error other: errorMessage shown
```

### Create Profile
```
User fills businessName + description → clicks "Create Profile"
  → saveProfile()
      → profileForm.invalid? → markAllAsTouched()
      → merchantService.createProfile(profileForm.value)
          → POST /api/merchant/profile
      → next: (data: MerchantResponse)
          → profile = data
          → successMessage = 'Profile created!'
          → timer(4000).subscribe(() => successMessage = '')
          → loadProducts()
```

Once `profile` is set, the template switches from the setup form to the full dashboard via `*ngIf="!loading && !profile"` / `*ngIf="profile"`.

### Add Product
```
User fills name, description, price → clicks "Add Product"
  → saveProduct()
      → productForm.invalid? → markAllAsTouched()
      → editingProductId is null → productService.create(productForm.value)
          → POST /api/product  [JWT required, Role=Merchant]
      → next:
          → productForm.reset()
          → editingProductId = null
          → successMessage = 'Product saved!'
          → timer(3000).subscribe(() => successMessage = '')
          → loadProducts()
```

### Edit Product
```
User clicks "Edit" on a product row
  → editProduct(product)
      → editingProductId = product.id
      → productForm.patchValue({ name, description, price })
      (form now shows product data, button label changes to "Update Product")

User edits and clicks "Update Product"
  → saveProduct()
      → editingProductId is set → productService.update(editingProductId, productForm.value)
          → PUT /api/product/{id}  [JWT required, Role=Merchant]
          → backend enforces ownership: product.MerchantId must match this user's merchant
```

### Load Products (Merchant's Own Only)
```
loadProducts()
  → productService.getByMerchant(profile.id)
      → GET /api/merchant/{profile.id}/products
```

`profile.id` is the Merchant entity ID returned from `GET /api/merchant/my-profile`. This is the merchant's own ID — so the product list is always scoped to their store. The merchant never sees another merchant's products in their dashboard.

---

## Feature 7 — Merchant Orders Received

**Route:** `/merchant/orders`
**Component:** `MerchantOrdersComponent`
**Guard:** Merchant only

### What it does
Shows all order line items where the product was sold by this merchant. Displays a summary card with total items sold and total revenue.

### ngOnInit
```
loadItems()
  → orderService.getMerchantItems()
      → GET /api/order/merchant-items  [JWT required, Role=Merchant]
      → backend: looks up merchant by JWT userId, queries OrderItems WHERE MerchantId = merchant.Id
  → next: items = data
```

### Revenue Calculation
```typescript
getTotalRevenue(): number {
  return this.items.reduce((sum, item) => sum + item.lineTotal, 0);
}
```
Called directly in the template via `{{ getTotalRevenue() | number:'1.2-2' }}`. This is a pure computation — no HTTP call needed since `lineTotal` is already in each `OrderItemResponse`.

---

## Full Customer Journey (Frontend)

```
1. Navigate to /auth
   → AuthComponent loads, isLoginMode = true

2. Register as Customer
   → POST /api/auth/register
   → storeSession() → localStorage
   → navigate to /catalog

3. Browse merchants
   → GET /api/merchant (no token needed)
   → Click merchant → GET /api/merchant/3/products

4. Add product to cart
   → POST /api/cart  [JWT attached by interceptor]
   → successMessage shown for 3 seconds

5. Navigate to /cart
   → GET /api/cart
   → Adjust quantities → PUT /api/cart/{id}
   → Remove item → DELETE /api/cart/{id}

6. Click "Proceed to Checkout"
   → POST /api/order/checkout
   → navigate to /payment/1

7. Payment page loads
   → POST /api/payment/initiate/1
   → Shows gateway URL

8. Click "Complete Payment"
   → POST /api/payment/complete/1
   → status card shown

9. Click "View My Orders"
   → navigate to /order/history
   → GET /api/order
   → Order shows Status: Paid
```

## Full Merchant Journey (Frontend)

```
1. Navigate to /auth
   → Register as Merchant
   → POST /api/auth/register
   → navigate to /merchant/products

2. Dashboard loads
   → GET /api/merchant/my-profile → 404
   → Setup form shown

3. Create profile
   → POST /api/merchant/profile
   → profile set, product form shown

4. Add products
   → POST /api/product (repeat for each product)

5. Edit a product
   → Click Edit → form pre-filled
   → PUT /api/product/{id}

6. Navigate to /merchant/orders
   → GET /api/order/merchant-items
   → See only items from their own products
```

---

## Change Detection and Zone.js

**How Angular knows to update the UI after HTTP responses:**

1. `zone.js` is loaded as a polyfill (listed in `angular.json` polyfills)
2. zone.js patches browser async APIs — `XMLHttpRequest`, `setTimeout`, `Promise`, etc.
3. When Angular's `HttpClient` makes a request, it runs inside the Angular zone
4. When the XHR response arrives, zone.js intercepts it and notifies Angular
5. Angular's change detection scheduler (wired by `provideZoneChangeDetection`) runs a CD cycle
6. Angular walks the component tree, compares old and new values, and updates the DOM

Without `provideZoneChangeDetection`, step 5 never happens — zone.js fires but Angular's scheduler is not listening. The component state updates correctly in memory but the DOM stays stale until the next user interaction triggers CD manually.

---

## HTTP Services Summary

| Service | Base URL | Key Methods |
|---|---|---|
| AuthService | `/api/auth` | `login()`, `register()`, `logout()` |
| MerchantService | `/api/merchant` | `getAll()`, `getMyProfile()`, `createProfile()` |
| ProductService | `/api` | `getByMerchant(id)`, `create()`, `update(id)` |
| CartService | `/api/cart` | `getCart()`, `addItem()`, `updateItem()`, `removeItem()` |
| OrderService | `/api/order` | `checkout()`, `getMyOrders()`, `getMerchantItems()` |
| PaymentService | `/api/payment` | `initiate(orderId)`, `complete(paymentId)`, `getStatus(orderId)` |

All services use `HttpClient` injected via constructor. All return `Observable<T>` — the component subscribes and handles `next` and `error` cases.
