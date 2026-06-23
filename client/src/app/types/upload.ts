

export type UploadState =
  | 'idle'
  | 'uploading'
  | 'success'
  | 'error';

export type FilterOption =
  | 'All'
  | CycleStatus;
export type CycleStatus = 'Pending' | 'Processed' | 'Done' | 'Error'|'Processing'|'Review Required';




