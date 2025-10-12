import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import {
  AdminDataService,
  ProductCreateDto,
  ProductReadDto,
} from '../../services/admin-data.service';
import { ImageUploadService } from '../../services/image-upload.service';

@Component({
  selector: 'app-admin-products',
  templateUrl: './admin-products.component.html',
  styleUrls: ['./admin-products.component.css'],
  encapsulation: ViewEncapsulation.None,
})
export class AdminProductsComponent implements OnInit {
  products: ProductReadDto[] = [];
  pagedProducts: ProductReadDto[] = [];

  showForm = false;
  editId: number | null = null;

  form: ProductCreateDto = {
    name: '',
    basePrice: 0,
    vatRate: 10,
    rx: false,
    manufacturer: '',
    category: '',
    description: '',
    imagePath: '',
  };

  currentPage = 1;
  pageSize = 12;
  totalPages = 1;

  selectedFile: File | null = null;
  imagePreview: string | null = null;
  isLoading = false;

  backendUrl = 'https://localhost:7201/';

  constructor(
    private admin: AdminDataService,
    private imageUpload: ImageUploadService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
    this.imagePreview = this.selectedFile
      ? URL.createObjectURL(this.selectedFile)
      : null;
  }

  openAddForm() {
    this.showForm = true;
  }

  closeForm() {
    this.showForm = false;
    this.resetForm();
  }

  load() {
    this.admin.getProducts().subscribe({
      next: (p) => {
        this.products = p;
        this.totalPages = Math.ceil(this.products.length / this.pageSize);
        this.updatePagedProducts();
      },
      error: (err) => console.error('Greška pri učitavanju proizvoda:', err),
    });
  }

  updatePagedProducts() {
    const start = (this.currentPage - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.pagedProducts = this.products.slice(start, end);
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.updatePagedProducts();
    }
  }

  prevPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.updatePagedProducts();
    }
  }

  submit() {
    this.isLoading = true;

    if (this.editId == null) {
      this.admin.createProduct(this.form).subscribe({
        next: (created) => {
          if (created && created.id && this.selectedFile) {
            this.uploadImage(created.id);
          } else {
            this.resetForm();
            this.load();
          }
        },
        error: (err) => console.error('Greška pri dodavanju proizvoda:', err),
        complete: () => (this.isLoading = false),
      });
    } else {
      this.admin.updateProduct(this.editId, this.form).subscribe({
        next: () => {
          if (this.selectedFile && this.editId) {
            this.uploadImage(this.editId);
          } else {
            this.cancelEdit();
            this.load();
          }
        },
        error: (err) => console.error('Greška pri izmeni proizvoda:', err),
        complete: () => (this.isLoading = false),
      });
    }
  }

  private uploadImage(productId: number) {
    if (!this.selectedFile) return;

    this.imageUpload.upload(productId, this.selectedFile).subscribe({
      next: (res) => {
        console.log('Upload uspešan:', res.imagePath);

        const product = this.products.find((p) => p.id === productId);
        if (product) product.imagePath = res.imagePath;

        this.resetForm();
        this.imagePreview = null;

        this.load();
      },
      error: (err) => console.error('Greška pri uploadu slike:', err),
    });
  }

  edit(p: ProductReadDto) {
    this.editId = p.id;
    this.showForm = true;
    this.form = {
      name: p.name,
      basePrice: p.basePrice,
      vatRate: p.vatRate,
      rx: p.rx,
      manufacturer: p.manufacturer,
      category: p.category,
      description: p.description,
      imagePath: p.imagePath,
    };
  }

  cancelEdit() {
    this.editId = null;
    this.resetForm();
    this.showForm = false;
  }

  resetForm() {
    this.form = {
      name: '',
      basePrice: 0,
      vatRate: 10,
      rx: false,
      manufacturer: '',
      category: '',
      description: '',
      imagePath: '',
    };
    this.selectedFile = null;
    this.imagePreview = null;
  }

  remove(id: number) {
    if (confirm('Da li sigurno želiš da obrišeš ovaj proizvod?')) {
      this.admin.deleteProduct(id).subscribe({
        next: () => this.load(),
        error: (err) => console.error('Greška pri brisanju proizvoda:', err),
      });
    }
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.src = this.backendUrl + 'images/default-product.jpg';
  }

  getImageUrl(path?: string): string {
    return path
      ? `https://localhost:7201/${path}`
      : 'https://localhost:7201/images/default-product.jpg';
  }
}
