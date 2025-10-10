export interface ProfileResponse {
  isCompany: boolean;
  id: string;
  name: string;
  email?: string;
  phone?: string;
  imageUrl?: string;
  local?: string;
  role?: string;
  isProfileCompleted?: boolean;
  isActive:number
}
