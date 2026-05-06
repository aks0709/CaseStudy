import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

// Interceptor to add JWT token to outgoing HTTP requests
export const jwtInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(AuthService).getToken();

  if (token) {
    return next(request.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    }));
  }

  return next(request);
};
