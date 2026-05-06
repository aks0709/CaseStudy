import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { OrderHistoryComponent } from './components/order-history.component';
import { AuthGuard } from '../shared/guards/auth.guard';
import { CustomerGuard } from '../shared/guards/customer.guard';

const routes: Routes = [
  { path: 'history', component: OrderHistoryComponent, canActivate: [AuthGuard, CustomerGuard] }
];

@NgModule({
  declarations: [OrderHistoryComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class OrderModule {}
