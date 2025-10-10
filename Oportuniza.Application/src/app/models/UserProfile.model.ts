export interface UserProfile {
  isCompany?: boolean;
  id: string;
  name: string;
  email?: string;
  phone?: string;
  imageUrl?: string;
  local?: string;
  interestArea?: string[];
  isProfileCompleted?: boolean;
  role?: string;
}
