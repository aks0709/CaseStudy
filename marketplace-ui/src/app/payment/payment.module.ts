import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { PaymentComponent } from './components/payment.component';
import { AuthGuard } from '../shared/guards/auth.guard';
import { CustomerGuard } from '../shared/guards/customer.guard';

const routes: Routes = [
  { path: ':orderId', component: PaymentComponent, canActivate: [AuthGuard, CustomerGuard] }
];

@NgModule({
  declarations: [PaymentComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class PaymentModule {}
