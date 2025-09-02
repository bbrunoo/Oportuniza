export interface PublicationFilterDto {
  searchTerm?: string;
  local?: string;
  contracts?: string[];   // agora pode faltar
  shifts?: string[];      // agora pode faltar
  salaryRange?: string | null;
}
