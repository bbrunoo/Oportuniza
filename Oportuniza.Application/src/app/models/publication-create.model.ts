export interface PublicationCreate {
  title: string;
  content: string;
  salary: string;
  shift: string;
  contract: string,
  local: string,
  expirationDate:string;
  tags:string[];
  postAsCompanyId?:string;
  cityId:string;
}
