import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { CatalogComponent } from './components/catalog.component';
import { AuthGuard } from '../shared/guards/auth.guard';
import { CustomerGuard } from '../shared/guards/customer.guard';

const routes: Routes = [
  { path: '', component: CatalogComponent, canActivate: [AuthGuard, CustomerGuard] }
];

@NgModule({
  declarations: [CatalogComponent],
  imports: [CommonModule, RouterModule.forChild(routes)]
})
export class CatalogModule {}
