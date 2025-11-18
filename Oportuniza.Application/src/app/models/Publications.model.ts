export interface Publication {
  hasApplied: boolean;
  id: string;
  title: string;
  description: string;
  creationDate: string;
  imageUrl: string;
  expired: boolean;
  authorId: string;
  authorType: number;
  authorName: string;
  postAuthorName:string;
  shift: string;
  local: string;
  resumee: string;
  expirationDate: string;
  contract: string;
  salary: string;
  authorImageUrl: string;
  expirationDateString?: string;
  cityId: string;
  isActive: number;
  companyOwnerId?: string;
}
