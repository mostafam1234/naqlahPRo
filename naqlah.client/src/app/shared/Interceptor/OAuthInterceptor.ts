import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AuthService } from "../services/auth.service";
@Injectable()
export class OAuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) { }
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (request.url.includes('/api') && !request.url.includes('/login') && !request.url.includes('/refresh')) {
      var isAcessTokenExpire = this.authService.CheckIfAcessTokenExpire();
      if (isAcessTokenExpire) {
        this.authService.RefreshToken().subscribe(data => {
          var acessToken = localStorage.getItem('acessToken');
          var acceptLangugae = localStorage.getItem("language");
          var nowDate = new Date();
          var timeOffsetInMinutes = (nowDate.getTimezoneOffset()) * (-1);
          const headers = request.headers.append('Authorization', `Bearer ${acessToken}`)
            .append('DateTimeOffset', `${timeOffsetInMinutes}`)
            .append('Accept-Language', `${acceptLangugae}`);
          const authReq = request.clone({ headers });
          return next.handle(authReq);
        });
      }

      else {

        var acessToken = localStorage.getItem('acessToken');
        var acceptLangugae = localStorage.getItem("language");
        var nowDate = new Date();
        var timeOffsetInMinutes = (nowDate.getTimezoneOffset()) * (-1);
        const headers = request.headers.append('Authorization', `Bearer ${acessToken}`)
          .append('DateTimeOffset', `${timeOffsetInMinutes}`)
          .append('Accept-Language', `${acceptLangugae}`);

        const authReq = request.clone({ headers });
        return next.handle(authReq);
      }


    }
    return next.handle(request);
  }
}


