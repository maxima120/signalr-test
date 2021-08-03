import { Component, Inject } from '@angular/core';
import { HttpClient, HttpEvent, HttpHandler, HttpRequest } from '@angular/common/http';
import { HttpTransportType, HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { stringify } from '@angular/compiler/src/util';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public forecasts: WeatherForecast[];
  hub: HubConnection;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {

    this.hub = new HubConnectionBuilder()
      .withUrl("/newshub")
      .build();

    this.hub.on("servermessage", (m: string) => { console.log(m); });

    this.hub.start()
      .then(() => {
        console.log(`MessageHub Connected: ${this.hub.connectionId}`);
        http.get<WeatherForecast[]>(baseUrl + 'weatherforecast/' + this.hub.connectionId).subscribe(result => {

          this.forecasts = result;

        }, error => console.log('Weather get error: ' + stringify(error)));

      })
      .catch(err => console.log('MessageHub connection error: ' + stringify(err)));
  }

  //intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

  //    const clonedReq = req.clone({
  //      headers: req.headers.set('Authorization', 'Bearer ' + localStorage.getItem('token'))
  //    });

  //    return next.handle(clonedReq).pipe(
  //      tap(
  //        succ => { },
  //        err => {
  //          console.log(stringify(err));
  //          if (err.status == 401) {
  //          //  localStorage.removeItem('token');
  //          //  localStorage.removeItem('username');
  //          //  this.router.navigateByUrl('/user/login');
  //          }
  //        }
  //      )
  //    )
  //}
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
