import { Component, OnInit } from '@angular/core';
import {
  AdminDataService,
  UserReadDto,
  UserCreateDto,
} from '../../services/admin-data.service';

@Component({
  selector: 'app-admin-users',
  templateUrl: './admin-users.component.html',
  styleUrls: ['./admin-users.component.css'],
})
export class AdminUsersComponent implements OnInit {
  users: UserReadDto[] = [];
  pagedUsers: UserReadDto[] = [];

  roles = ['Admin', 'Customer'];

  newUser: UserCreateDto = {
    email: '',
    fullName: '',
    password: '',
    role: 'Customer',
  };
  showForm = false;

  // paginacija
  currentPage = 1;
  pageSize = 20;
  totalPages = 1;

  constructor(private admin: AdminDataService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers() {
    this.admin.getUsers().subscribe((u) => {
      this.users = u;
      this.totalPages = Math.ceil(this.users.length / this.pageSize);
      this.setPage(1);
    });
  }

  setPage(page: number) {
    this.currentPage = page;
    const start = (page - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.pagedUsers = this.users.slice(start, end);
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.setPage(this.currentPage + 1);
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.setPage(this.currentPage - 1);
    }
  }

  toggleForm() {
    this.showForm = !this.showForm;
  }

  createUser() {
    this.admin.createUser(this.newUser).subscribe(() => {
      this.newUser = {
        email: '',
        fullName: '',
        password: '',
        role: 'Customer',
      };
      this.showForm = false;
      this.loadUsers();
    });
  }

  changeRole(userId: string, role: string) {
    this.admin.updateUserRole(userId, role).subscribe(() => this.loadUsers());
  }

  deleteUser(userId: string) {
    if (confirm('Da li si siguran da želiš da obrišeš korisnika?')) {
      this.admin.deleteUser(userId).subscribe(() => this.loadUsers());
    }
  }
}
