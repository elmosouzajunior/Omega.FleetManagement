import { LOCALE_ID, NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { CommonModule, registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt'; 
import { BrowserModule, provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { RouterModule } from '@angular/router'; // <--- Adicionado
import { NgApexchartsModule } from 'ng-apexcharts';

import { AppRoutingModule } from './app-routing-module';
import { App } from './app';

// Importações dos componentes
import { LoginComponent } from './pages/login/login'; 
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { MasterLayoutComponent } from './components/layouts/master-layout/master-layout';
import { CompanyListComponent } from './pages/master/company-list/company-list';
import { CompanyCreateComponent } from './pages/master/company-create/company-create';
import { UsersListComponent } from './pages/master/user-list/user-list';
import { UserCreateComponent } from './pages/master/user-create/user-create';
import { AdminDashboardComponent } from './pages/admin/admin-dashboard/admin-dashboard';
import { AdminLayoutComponent } from './components/layouts/admin-layout/admin-layout';
import { SidebarComponent } from './components/layouts/sidebar/sidebar';
import { VehicleListComponent } from './pages/admin/vehicle-list/vehicle-list';
import { VehicleCreateComponent } from './pages/admin/vehicle-create/vehicle-create';
import { DriverListComponent } from './pages/admin/driver-list/driver-list';
import { DriverCreateComponent } from './pages/admin/driver-create/driver-create';
import { TripListComponent } from './pages/admin/trip-list/trip-list';
import { TripOpenComponent } from './pages/admin/trip-open/trip-open';
import { TripDetailComponent } from './pages/admin/trip-detail/trip-detail';
import { TripExpenseCreateComponent } from './pages/admin/trip-expense-create/trip-expense-create';
import { VehicleExpenseCreateComponent } from './pages/admin/vehicle-expense-create/vehicle-expense-create';
import { ExpenseTypeListComponent } from './pages/master/expense-type-list/expense-type-list';
import { ProductListComponent } from './pages/master/product-list/product-list';
import { ReceiptDocumentTypeListComponent } from './pages/master/receipt-document-type-list/receipt-document-type-list';
import { ReportCostKmComponent } from './pages/admin/report-cost-km/report-cost-km';
import { ReportProfitVehicleComponent } from './pages/admin/report-profit-vehicle/report-profit-vehicle';

registerLocaleData(localePt, 'pt-BR');

@NgModule({
  declarations: [
    App,
    LoginComponent, 
    MasterLayoutComponent,
    CompanyListComponent,
    CompanyCreateComponent,
    UsersListComponent,
    UserCreateComponent,
    AdminDashboardComponent,
    DriverListComponent,
    DriverCreateComponent,
    VehicleListComponent,
    VehicleCreateComponent,
    SidebarComponent,
    AdminLayoutComponent,
    TripListComponent,
    TripOpenComponent,
    TripDetailComponent,
    TripExpenseCreateComponent,
    VehicleExpenseCreateComponent,
    ReportCostKmComponent,
    ReportProfitVehicleComponent,
    ExpenseTypeListComponent,
    ProductListComponent,
    ReceiptDocumentTypeListComponent,
  ],
  imports: [
    BrowserModule,
    CommonModule,
    RouterModule, // <--- Adicionado para garantir o router-outlet
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    NgApexchartsModule
  ],
  providers: [
    { provide: LOCALE_ID, useValue: 'pt-BR' },
    provideBrowserGlobalErrorListeners(),
    provideClientHydration(withEventReplay()),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    )
  ],
  bootstrap: [App]
})
export class AppModule { }

