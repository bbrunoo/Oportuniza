export interface UserProfile {
  id: string;
  name: string;
  email: string;
  phone: string;
  imageUrl: string;
  local:string;
  interestArea:string[];
  isProfileCompleted: boolean;
}
