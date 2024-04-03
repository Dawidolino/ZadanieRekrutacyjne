import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tag } from '../models/tag';

@Injectable({
  providedIn: 'root'
})
export class TagService {
  private apiUrl = 'api/gettags';

  constructor(private http: HttpClient) { }

  getTags(totalTags: number, currentPage: number, sortBy: string, sortOrder: string): Observable<Tag[]> {
    let params = new HttpParams()
      .set('totalTags', totalTags.toString())
      .set('currentPage', currentPage.toString())
      .set('sortBy', sortBy)
      .set('sortOrder', sortOrder);

    return this.http.get<Tag[]>(this.apiUrl, { params });
  }
}
