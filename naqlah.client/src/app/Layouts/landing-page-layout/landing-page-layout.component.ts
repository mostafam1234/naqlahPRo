import { Component } from '@angular/core';
import { PublicHeaderComponent } from 'src/app/Pages/landing-page/public-header/public-header.component';
import { RouterModule } from "@angular/router";
import { PublicFooterComponent } from "src/app/Pages/landing-page/public-footer/public-footer.component";

@Component({
  selector: 'app-landing-page-layout',
  standalone: true,
  imports: [PublicHeaderComponent, RouterModule, PublicFooterComponent],
  templateUrl: './landing-page-layout.component.html',
  styleUrl: './landing-page-layout.component.css'
})
export class LandingPageLayoutComponent {

}
