import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tag } from '../models/tag';
import { HttpClientModule } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
@Injectable({
  providedIn: 'root'
})
export class TagService {

  url: string = environment.apiBaseUrl+'/gettags'
  list:Tag[]=[]
  
  constructor(private http: HttpClient) { }
    
  getTags(totalTags: number, currentPage: number, sortBy: string, sortOrder: string): Observable<Tag[]> {
    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        // accept self-written certificate
        'rejectUnauthorized': 'false'
      })
    };

    let params = new HttpParams()
      .set('totalTags', totalTags.toString())
      .set('currentPage', currentPage.toString())
      .set('sortBy', sortBy)
      .set('sortOrder', sortOrder);

    return this.http.get<Tag[]>(this.url, { params, ...httpOptions});
  }
}
