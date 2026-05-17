import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { API_BASE_URL } from './api-client';

export interface AuthCredentials {
  username: string;
  password: string;
}

interface TokenResponse {
  token: string;
}

const TOKEN_KEY = 'kros.auth.token';
const USERNAME_KEY = 'kros.auth.username';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  private readonly _token = signal<string | null>(this.readToken());
  private readonly _username = signal<string | null>(localStorage.getItem(USERNAME_KEY));

  readonly token = this._token.asReadonly();
  readonly username = this._username.asReadonly();
  readonly isLoggedIn = computed(() => this._token() !== null);

  register(credentials: AuthCredentials): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.baseUrl}/api/Auth/register`, credentials)
      .pipe(tap(res => this.setSession(res.token, credentials.username)));
  }

  login(credentials: AuthCredentials): Observable<TokenResponse> {
    return this.http
      .post<TokenResponse>(`${this.baseUrl}/api/Auth/login`, credentials)
      .pipe(tap(res => this.setSession(res.token, credentials.username)));
  }

  logout(): Observable<unknown> {
    return this.http
      .post(`${this.baseUrl}/api/Auth/logout`, {})
      .pipe(tap(() => this.clearSession()));
  }

  private setSession(token: string, username: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USERNAME_KEY, username);
    this._token.set(token);
    this._username.set(username);
  }

  private clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USERNAME_KEY);
    this._token.set(null);
    this._username.set(null);
  }

  private readToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }
}
