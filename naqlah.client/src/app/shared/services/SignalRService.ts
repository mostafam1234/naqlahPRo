import { Injectable, NgZone } from "@angular/core";
import { AppConfigService } from "./AppConfigService";
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { BehaviorSubject, Subject, Observable, filter, take } from "rxjs";
import { NotificationDto } from "src/app/Core/services/NaqlahClient";



@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  connection: HubConnection | null = null;
  notificationConnection: HubConnection | null = null;
  isConnected: boolean = false;
  private orderIdSubject: BehaviorSubject<number> = new BehaviorSubject<number>(0);
  private notificationSubject: Subject<NotificationDto> = new Subject<NotificationDto>();
  
  constructor(
    private appConfigService: AppConfigService,
    private ngZone: NgZone
  ) {
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

  public StartNotificationConnection(accessToken: string) {
    if (this.notificationConnection) {
      return;
    }

    
    // Check if config is already loaded
    if (this.appConfigService.loaded$.value) {
      this.proceedWithConnection(accessToken);
      return;
    }

    
    // Wait for config to load from appSettings.json
    this.appConfigService.loaded$
      .pipe(
        filter(loaded => {
          return loaded === true;
        }),
        take(1)
      )
      .subscribe(() => {
        this.proceedWithConnection(accessToken);
      });
  }

  private proceedWithConnection(accessToken: string) {
    debugger;
    var config = this.appConfigService.getConfig();
    var url = config.apiBaseUrl;
    
    // Remove trailing slash if present
    url = (url || '').replace(/\/$/, '');
    
    // apiBaseUrl must be configured in assets/appSettings.json
    // For production, update appSettings.json with your production backend URL
    if (!url || url.trim() === '') {
      return;
    }
    
    var hubUrl = `${url}/NotificationHub`;
    
    this.createNotificationConnection(hubUrl, accessToken);
  }

  private createNotificationConnection(hubUrl: string, accessToken: string) {
    var connection = new HubConnectionBuilder()
      .configureLogging(LogLevel.Information)
      .withUrl(hubUrl, {
        accessTokenFactory: () => {
          const token = localStorage.getItem('accessToken') || accessToken;
          console.log('SignalR access token factory called, token exists:', !!token);
          console.log('Token length:', token ? token.length : 0);
          if (!token) {
            console.error('❌ No access token available for SignalR connection!');
          }
          return Promise.resolve(token || '');
        },
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    // Register the event handler BEFORE starting the connection
    connection.on('NewNotification', (notification: any) => {
      
      // Use NgZone to ensure Angular change detection runs
      this.ngZone.run(() => {
        // Send notification data as-is (contains ArabicTitle/EnglishTitle with capital letters)
        // NotificationService will handle the mapping to title/message based on language
        this.notificationSubject.next(notification);
      });
    });

    // Store connection before starting
    this.notificationConnection = connection;

    connection.start().then(() => {
      this.isConnected = true;
    }).catch((err) => {
      console.error('❌ Notification SignalR connection error:', err);

      this.isConnected = false;
      this.notificationConnection = null;
    });

    connection.onreconnecting(() => {
      console.log('Notification SignalR reconnecting...');
    });

    connection.onreconnected(() => {
      console.log('Notification SignalR reconnected!');
    });

    connection.onclose((error) => {
      console.log('🔴 Notification SignalR connection closed', error);
      this.isConnected = false;
      this.notificationConnection = null;
    });
  }

  public StopNotificationConnection() {
    if (this.notificationConnection) {
      this.notificationConnection.stop().then(() => {
        console.log('Notification SignalR connection stopped');
        this.notificationConnection = null;
        this.isConnected = false;
      }).catch((err) => {
        console.error('Error stopping Notification SignalR connection:', err);
      });
    }
  }

  GetHubConnection(): HubConnection {
    return this.connection as HubConnection;
  }

  public ListenForNewOrder() {
    return this.orderIdSubject.asObservable();
  }

  public ListenForNotifications(): Observable<any> {
    return this.notificationSubject.asObservable();
  }

}
