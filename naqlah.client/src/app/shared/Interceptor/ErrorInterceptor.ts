import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { catchError, EMPTY, from, Observable, switchMap, tap, throwError } from "rxjs";
import { AuthService } from "../services/auth.service";
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    constructor(private authService :  AuthService,private router : Router){}
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!request.url.includes('/api')) {
      return next.handle(request);
    }

    return next.handle(request)
      .pipe(catchError(error => {
        if (error instanceof HttpResponse && error.status === 401) {
          return this.Handler401Error(request, next);
        }
        else {
          return next.handle(request)
        }
      }))
  }

  private Handler401Error(request: HttpRequest<any>,
    next: HttpHandler): Observable<HttpEvent<any>> {
    return this.authService.RefreshToken()
      .pipe(switchMap(() => {
        return next.handle(this.addToken(request))
      }),
        catchError(error => {
          this.authService.LogOut();
          this.router.navigate(['/']);
          return throwError(() => error)
        }))
  }


  private addToken(request: HttpRequest<any>): HttpRequest<any> {
    const acessToken = localStorage.getItem("acessToken");
    if (acessToken) {
      const headers = request.headers.append('Authorization', `Bearer ${acessToken}`);
      const authReq = request.clone({ headers });
      return authReq;
    }

    return request;
  }


  }
    
    
