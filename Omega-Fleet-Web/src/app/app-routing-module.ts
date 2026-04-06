import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login'; 
import { MasterLayoutComponent } from './components/layouts/master-layout/master-layout';
import { CompanyListComponent } from './pages/master/company-list/company-list';
import { CompanyCreateComponent } from './pages/master/company-create/company-create';
import { authGuard } from './core/guards/auth-guard';
import { UsersListComponent } from './pages/master/user-list/user-list';
import { UserCreateComponent } from './pages/master/user-create/user-create'
import { AdminDashboardComponent } from './pages/admin/admin-dashboard/admin-dashboard';
import { RenderMode } from '@angular/ssr';
import { DriverListComponent } from './pages/admin/driver-list/driver-list';
import { DriverCreateComponent } from './pages/admin/driver-create/driver-create';
import { AdminLayoutComponent } from './components/layouts/admin-layout/admin-layout';
import { VehicleListComponent } from './pages/admin/vehicle-list/vehicle-list';
import { VehicleCreateComponent } from './pages/admin/vehicle-create/vehicle-create';
import { TripOpenComponent } from './pages/admin/trip-open/trip-open';
import { TripListComponent } from './pages/admin/trip-list/trip-list';
import { TripDetailComponent } from './pages/admin/trip-detail/trip-detail';
import { TripExpenseCreateComponent } from './pages/admin/trip-expense-create/trip-expense-create';
import { VehicleExpenseCreateComponent } from './pages/admin/vehicle-expense-create/vehicle-expense-create';
import { ExpenseTypeListComponent } from './pages/master/expense-type-list/expense-type-list';
import { ProductListComponent } from './pages/master/product-list/product-list';
import { ReceiptDocumentTypeListComponent } from './pages/master/receipt-document-type-list/receipt-document-type-list';
import { ReportCostKmComponent } from './pages/admin/report-cost-km/report-cost-km';


const routes: Routes = [
  { path: 'login', component: LoginComponent },  
  {
    path: 'Master', // Deve bater com o que está no LoginComponent
    component: MasterLayoutComponent,
    canActivate: [authGuard], // Protege a rota
    data: { role: 'Master' },
    children: [
      { path: 'companies', component: CompanyListComponent },
      { path: 'companies/new', component: CompanyCreateComponent },
      { path: 'expense-types', component: ExpenseTypeListComponent },
      { path: 'products', component: ProductListComponent },
      { path: 'receipt-document-types', component: ReceiptDocumentTypeListComponent },
      { path: 'users', component: UsersListComponent },
      { path: 'users/new', component: UserCreateComponent }
    ]
  },
  { 
    path: 'Admin',
    component: AdminLayoutComponent, 
    canActivate: [authGuard], 
    data: { role: 'CompanyAdmin' }, // Só Administrador entra aqui
    children: [
       { path: 'dashboard', component: AdminDashboardComponent, canActivate: [authGuard], data: { RenderMode: 'browser' }},
       { path: 'drivers', component: DriverListComponent },
       { path: 'driver-create', component: DriverCreateComponent },
       { path: 'vehicles', component: VehicleListComponent },
       { path: 'vehicle-create', component: VehicleCreateComponent },
       { path: 'trips', component: TripListComponent },
       { path: 'trip-open', component: TripOpenComponent },
       { path: 'trip-detail/:id', component: TripDetailComponent },
       { path: 'trip-expenses/new', component: TripExpenseCreateComponent },
       { path: 'vehicle-expenses/new', component: VehicleExpenseCreateComponent },
       { path: 'reports', component: ReportCostKmComponent },
       { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
       // ... outras rotas do Admin da Empresa
    ]
  },
  
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' } // Rota de fallback para evitar erros de 404 no front
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

