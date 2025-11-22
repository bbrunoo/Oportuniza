export interface PublicationFilterDto {
  searchTerm?: string;
  local?: string;
  contracts?: string[];
  shifts?: string[];
  salaryRange?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  radiusKm?: number | null;
}
