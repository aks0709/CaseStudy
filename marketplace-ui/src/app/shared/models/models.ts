//They mirror backend Dtos exactly, so that we can easily map API responses to these interfaces in the frontend.
// Auth models
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  fullName: string;
  email: string;
  role: string;
}

// Merchant models
export interface MerchantResponse {
  id: number;
  userId: number;
  businessName: string;
  description: string;
  createdAt: string;
}

export interface CreateMerchantRequest {
  businessName: string;
  description: string;
}

// Product models
export interface ProductResponse {
  id: number;
  merchantId: number;
  name: string;
  description: string;
  price: number;
  createdAt: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
}

// Cart models
export interface CartItemResponse {
  id: number;
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface CartResponse {
  items: CartItemResponse[];
  grandTotal: number;
}

export interface AddCartItemRequest {
  productId: number;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}

// Order models
export interface OrderItemResponse {
  id: number;
  orderId: number;
  productId: number;
  productName: string;
  merchantId: number;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderResponse {
  id: number;
  userId: number;
  totalAmount: number;
  status: string;
  placedAt: string;
  items: OrderItemResponse[];
}

// Payment models
export interface InitiatePaymentResponse {
  paymentId: number;
  orderId: number;
  status: string;
  paymentUrl: string;
  initiatedAt: string;
}

export interface PaymentStatusResponse {
  paymentId: number;
  orderId: number;
  paymentStatus: string;
  orderStatus: string;
  initiatedAt: string;
  completedAt: string | null;
}
