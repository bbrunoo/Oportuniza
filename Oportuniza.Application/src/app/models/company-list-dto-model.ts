export interface CompanyListDto {
  id: string;
  name: string;
  description: string;
  imageUrl: string
  cityState: string;
  phone: string;
  email: string;
  cnpj: string;
  UserRole: string;
  OwnerId: string;
  isActive: number;
  IsDisabled?: boolean;
  IsActiveStatus?: boolean;
}
