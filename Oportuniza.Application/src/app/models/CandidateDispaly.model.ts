export interface CandidateDisplay {
  publicationId: string;
  title: string;
  resumee: string;
  authorImage: string;
  authorName: string;
  candidates: {
    applicationId: string;
    userId: string;
    userName: string;
    userImage: string;
    status: string;
    createdAt: string;
  }[];
}
