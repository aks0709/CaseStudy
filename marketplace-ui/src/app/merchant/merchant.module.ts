import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { MerchantDashboardComponent } from './components/merchant-dashboard.component';
import { MerchantOrdersComponent } from './components/merchant-orders.component';
import { AuthGuard } from '../shared/guards/auth.guard';
import { MerchantGuard } from '../shared/guards/merchant.guard';

const routes: Routes = [
  { path: 'dashboard', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: MerchantDashboardComponent, canActivate: [AuthGuard, MerchantGuard] },
  { path: 'orders', component: MerchantOrdersComponent, canActivate: [AuthGuard, MerchantGuard] }
];

@NgModule({
  declarations: [MerchantDashboardComponent, MerchantOrdersComponent],
  imports: [CommonModule, ReactiveFormsModule, RouterModule.forChild(routes)]
})
export class MerchantModule {}
