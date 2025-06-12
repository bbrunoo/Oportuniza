export interface Publication {
  id: string;
  title: string;
  description: string;
  creationDate: string;
  imageUrl: string;
  expired: boolean;
  authorId: string;
  authorType: number;
  authorName: string;
  authorImageUrl: string;
}
