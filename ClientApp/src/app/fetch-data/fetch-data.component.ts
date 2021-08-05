import { Component, Inject } from '@angular/core';
import { HttpClient, HttpEvent, HttpHandler, HttpRequest } from '@angular/common/http';
import { HttpTransportType, HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { formatDate } from '@angular/common';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public forecasts: WeatherForecast[];
  hub: HubConnection;

  count: number;
  startSw: number;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {

    this.hub = new HubConnectionBuilder()
      .withUrl("/newshub")
      .build();

    this.count = 0;

    this.hub.on("servermessage", (m: string) =>
    {
      //console.log(`${formatDate(new Date(Date.now()), "HH:mm:ss.SSSSSS", "en-GB")} ${m}`);
      if (this.count == 0)
        this.startSw = Date.now();

      this.count++;

      if (this.count == 1000) {
        console.log(`${Date.now() - this.startSw}`);
        console.log(`${formatDate(new Date(Date.now()), "HH:mm:ss.SSSSSS", "en-GB")} ${m}`);
      }

    });

    this.hub.start()
      .then(() => {
        console.log(`MessageHub Connected: ${this.hub.connectionId}`);
        http.get<WeatherForecast[]>(baseUrl + 'weatherforecast/' + this.hub.connectionId).subscribe(result => {

          this.forecasts = result;

        }, error => console.log('Weather get error: ' + error.toString()));

      })
      .catch(err => console.log('MessageHub connection error: ' + err.toString()));
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
