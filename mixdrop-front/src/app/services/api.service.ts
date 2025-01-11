import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { lastValueFrom, Observable } from 'rxjs';
import { Result } from '../models/result';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private BASE_URL = environment.apiImg;

  jwt: string | null = ""

  constructor(private http: HttpClient) {
    let token: string | null = localStorage.getItem("token")
    if (token) {
      this.jwt = token
    }
    /*if(localStorage.getItem("remember") == "false")
    {
      this.deleteToken()
    }*/
  }

  deleteToken() {
    this.jwt = null;
    localStorage.removeItem("token");
    localStorage.removeItem("shoppingCart")
  }

  async get<T = void>(path: string, params: any = {}, responseType: any = null): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.get(url, {
      params: new HttpParams({ fromObject: params }),
      headers: this.getHeader(),
      responseType: responseType,
      observe: 'response',
    });
    return this.sendRequest<T>(request$);
  }

  async post<T = void>(path: string, body: Object = {}, params: any = {}): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.post(url, body, {
      params: new HttpParams({ fromObject: params }),
      headers: this.getHeader(null),
      observe: 'response',
      responseType: 'text'
    });

    if(path.includes("login") || path.includes("sign"))
    {
      return this.sendRequest<T>(request$, true);
    }
    else
    {
      return this.sendRequest<T>(request$);
    }
  }

  async postWithImage<T = void>(path: string, body: Object = {}): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.post(url, body, {
      headers: this.getHeader(null, ""),
      observe: 'response',
      responseType: 'text'
    });
    return this.sendRequest<T>(request$);
  }

  async put<T = void>(path: string, body: Object = {}, contentType = null): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.put(url, body, {
      headers: this.getHeader(contentType),
      observe: 'response'
    });

    return this.sendRequest<T>(request$);
  }

  async putWithImage<T = void>(path: string, body: Object = {}, contentType = null): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.put(url, body, {
      headers: this.getHeader(contentType, ""),
      observe: 'response'
    });
    return this.sendRequest<T>(request$);
  }

  async delete<T = void>(path: string, params: any = {}): Promise<Result<T>> {
    const url = `${this.BASE_URL}${path}`;
    const request$ = this.http.delete(url, {
      params: new HttpParams({ fromObject: params }),
      headers: this.getHeader(),
      observe: 'response'
    });

    return this.sendRequest<T>(request$);
  }

  private async sendRequest<T = boolean>(request$: Observable<HttpResponse<any>>, saveJwt: boolean = false): Promise<Result<T>> {
    let result: Result<T>;

    try {
      const response = await lastValueFrom(request$);
      const statusCode = response.status;

      if (response.ok) {
        const data = response.body as T;

        if (data == undefined) {
          result = Result.success(statusCode);
        } else {
          result = Result.success(statusCode, data);
        }
      } else {
        result = result = Result.error(statusCode, response.statusText);
      }

    } catch (exception: any) {
      console.log("EXCEPCION: ", exception)
      if (exception instanceof HttpErrorResponse) {
        result = Result.error(exception.status, exception.statusText);
      } else {
        result = Result.error(-1, exception.mesage);
      }
    }

    console.log("RESULT: ", result)
    if (result.data && saveJwt) {
      this.jwt = result.data.toString();
      console.log("AY MI MADRE EL BICHO: ", this.jwt)
    }
    return result;
  }

  private getHeader(accept = null, contentType = "application/json"): HttpHeaders {
    let header: any = {};

    // Para cuando haya que poner un JWT
    console.log("JWT: ", this.jwt)
    header['Authorization'] = `Bearer ${this.jwt}`;

    if (accept)
      header['Accept'] = accept;

    if (contentType && contentType !== "")
      header['Content-Type'] = contentType;

    const headerObject = new HttpHeaders(header)

    console.log("HEADER OBJECT: ", headerObject);

    return headerObject;
  }
}
