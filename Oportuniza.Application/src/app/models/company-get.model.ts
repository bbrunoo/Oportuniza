export interface CompanyEmployeeDto {
  id: string;
  userId: string;
  userName: string;
  roles: 'Owner' | 'Manager' | 'Employee' | string;
  canPostJobs: boolean;
  userEmail: string;
  imageUrl: string;
  isActive: number;
}


export interface CompanyDto {
  id: string;
  name: string;
  description: string | null;
  active: boolean;
  userId: string;
  managerName: string | null;
  imageUrl: string;
  createdAt: string;
  cityState: string;
  phone: string;
  email: string;
  cnpj: string;
  employees: CompanyEmployeeDto[];
}
