import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { CartComponent } from './components/cart.component';
import { AuthGuard } from '../shared/guards/auth.guard';
import { CustomerGuard } from '../shared/guards/customer.guard';

const routes: Routes = [
  { path: '', component: CartComponent, canActivate: [AuthGuard, CustomerGuard] }
];

@NgModule({
  declarations: [CartComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class CartModule {}
