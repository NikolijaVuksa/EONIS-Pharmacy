import { Component, OnInit } from '@angular/core';
import {
  AdminDataService,
  UserCreateDto,
  UserReadDto,
} from '../services/admin-data.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
})
export class UsersComponent implements OnInit {
  users: UserReadDto[] = [];
  showForm = false;
  editMode = false;
  editId: string | null = null;

  form: UserCreateDto = {
    email: '',
    fullName: '',
    password: '',
    role: 'Customer',
  };

  searchTerm = '';

  constructor(private adminService: AdminDataService) {}

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.adminService.getUsers().subscribe((res) => {
      this.users = res;
    });
  }

  openAddForm() {
    this.showForm = true;
    this.editMode = false;
    this.editId = null;
    this.form = { email: '', fullName: '', password: '', role: 'Customer' };
  }

  editUser(user: UserReadDto) {
    this.showForm = true;
    this.editMode = true;
    this.editId = user.id;
    this.form = {
      email: user.email,
      fullName: user.fullName,
      password: '',
      role: user.roles[0],
    };
  }

  submit() {
    if (this.editMode && this.editId) {
      this.adminService
        .updateUserRole(this.editId, this.form.role)
        .subscribe(() => {
          this.loadUsers();
          this.showForm = false;
        });
    } else {
      this.adminService.createUser(this.form).subscribe(() => {
        this.loadUsers();
        this.showForm = false;
      });
    }
  }

  deleteUser(id: string) {
    if (confirm('Da li sigurno želiš da obrišeš korisnika?')) {
      this.adminService.deleteUser(id).subscribe(() => {
        this.loadUsers();
      });
    }
  }

  get filteredUsers() {
    return this.users.filter(
      (u) =>
        u.fullName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        u.email.toLowerCase().includes(this.searchTerm.toLowerCase())
    );
  }
}
