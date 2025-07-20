import { Component } from '@angular/core';
import { AboutComponent } from "../about/about.component";
import { VisionComponent } from "../vision/vision.component";
import { ServicesComponent } from "../services/services.component";
import { DownloadComponent } from "../download/download.component";
import { ContactUsComponent } from "../contact-us/contact-us.component";

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [AboutComponent, VisionComponent, ServicesComponent, DownloadComponent, ContactUsComponent],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css'
})
export class HomePageComponent {

}
