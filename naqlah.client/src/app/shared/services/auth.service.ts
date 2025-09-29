import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { EMPTY, Observable, catchError, map } from 'rxjs';
import { LoginResponse } from '../Models/login_response';
import { AppConfigService } from './AppConfigService';
import { Client, LoginRequest, RefreshRequest } from 'src/app/Core/services/NaqlahClient';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private httpClient: HttpClient,
    private appConfigService: AppConfigService,
    private cleint: Client) { }



  Login(credential: LoginRequest): Observable<LoginResponse> {
    var apiBaseUrl = this.appConfigService.getConfig().apiBaseUrl;
    return this.httpClient.post<LoginResponse>(`${apiBaseUrl}/login`, credential)
      .pipe(map(response => {
        localStorage.setItem("acessToken", response.accessToken);
        localStorage.setItem("refreshToken", response.refreshToken);
        const expirationTime = new Date().getTime() + response.expiresIn * 1000;
        localStorage.setItem('expirationTime', expirationTime.toString());
        return response;
      }), catchError(err => {
        console.log(err);
        return EMPTY;
      }));
  }



  RefreshToken(): Observable<LoginResponse> {
    var apiBaseUrl = this.appConfigService.getConfig().apiBaseUrl;
    const refreshToken = this.GetRefreshTokenFromLocalStorage();
    var refreshRequest = new RefreshRequest();
    refreshRequest.refreshToken = refreshToken as string;
    return this.httpClient.post<LoginResponse>(`${apiBaseUrl}/refresh`, refreshRequest)
      .pipe(map(response => {
        localStorage.setItem("acessToken", response.accessToken);
        localStorage.setItem("refreshToken", response.refreshToken);
        var expirationTime = new Date().getTime() + response.expiresIn * 1000;
        localStorage.setItem('expirationTime', expirationTime.toString());
        return response;
      }));
  }


  CheckIfAcessTokenExpire(): boolean {
    const expirationTime = localStorage.getItem('expirationTime');
    if (!expirationTime) {
      return true;
    }
    const now = new Date().getTime();
    const parsedExpirationTime = parseInt(expirationTime, 10);
    if (isNaN(parsedExpirationTime)) {
      return true;
    }
    return now > parsedExpirationTime;
  }


  LogOut(): void {
    localStorage.removeItem('acessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('expirationTime');
  }


  IsLoggedIn(): boolean {
    var acessToken = localStorage.getItem('acessToken');
    var refreshToken = this.GetRefreshTokenFromLocalStorage();
    return acessToken !== null && refreshToken !== null;
  }

  public GetAcessToken(): string {
    var acessToken = localStorage.getItem('acessToken') as string;
    return acessToken;
  }

  public GetUserRole(): string {
    var userRole = localStorage.getItem("role") as string;
    return userRole;
  }



  public GetRefreshTokenFromLocalStorage(): string | null {

    var refreshToken = localStorage.getItem("refreshToken");

    return refreshToken;
  }
}
