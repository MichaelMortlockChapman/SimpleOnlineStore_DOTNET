import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth/auth-service';

@Component({
  selector: 'app-sidebar',
  imports: [ RouterLink ],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css'],
})
export class Sidebar {
  private authService = inject(AuthService)

  username = "Jackson"
  loggedin = this.authService.login()
}
