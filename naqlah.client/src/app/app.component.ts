import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { ToasterComponent } from './shared/components/toaster/toaster.component';
import { ThemeService } from './shared/services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule, ToasterComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  constructor(private themeService: ThemeService) {}
}
