import { Component, OnInit } from '@angular/core';
import { AdminDataService } from '../../services/admin-data.service';

@Component({
  selector: 'app-admin-users',
  templateUrl: './admin-users.component.html',
})
export class AdminUsersComponent implements OnInit {
  users: any[] = [];
  constructor(private admin: AdminDataService) {}

  ngOnInit(): void {
    this.admin.getUsers().subscribe((u) => (this.users = u));
  }
}
