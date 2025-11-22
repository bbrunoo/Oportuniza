export interface PublicationCreate {
  title: string;
  description: string;
  salary: string;
  shift: string;
  contract: string,
  local: string,
  expirationDate: string;
  cityId: string;
  postAsCompanyId?: string | null;
}
