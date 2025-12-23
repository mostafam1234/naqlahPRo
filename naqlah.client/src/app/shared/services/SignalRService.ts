import { Injectable } from "@angular/core";
import { AppConfigService } from "./AppConfigService";
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { BehaviorSubject } from "rxjs";



@Injectable()


export class SignalRService {
  connection: HubConnection | null = null;
  isConnected: boolean = false;
  private orderIdSubject: BehaviorSubject<number> = new BehaviorSubject<number>(0);
  constructor(private appConfigService: AppConfigService) {
  }


  public StartConnection(acessToken: string) {
    if (this.connection) {
      return;
    }

    var url = this.appConfigService.getConfig().apiBaseUrl;

    var connection = new HubConnectionBuilder()
      .configureLogging(LogLevel.Debug)
      .withUrl(`${url}/OperationHub`, {
        accessTokenFactory: () => acessToken as string,
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets
      })
      .build();


    connection.on('NewOrder', (orderId: number) => {
      this.orderIdSubject.next(orderId);
    });


    connection.start().then(() => {
      console.log('SignalR Connected!');
      
    }).catch((err) => {
      console.error(err.toString());
    });


    this.connection = connection;

  }

  GetHubConnection(): HubConnection {
    return this.connection as HubConnection;
  }

  public ListenForNewOrder() {
    return this.orderIdSubject.asObservable();
  }



}
