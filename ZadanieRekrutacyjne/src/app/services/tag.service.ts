import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
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

  // refreshList(){
  //   this.http.get(this.url)
  //   .subscribe({
  //     next:res=>{
  //       this.list=res as Tag[]
  //     },
  //     error: err=> {console.log(err)}
  //   })
  // }
  getTags(totalTags: number, currentPage: number, sortBy: string, sortOrder: string): Observable<Tag[]> {
    let params = new HttpParams()
      .set('totalTags', totalTags.toString())
      .set('currentPage', currentPage.toString())
      .set('sortBy', sortBy)
      .set('sortOrder', sortOrder);

    return this.http.get<Tag[]>(this.url, { params });
  }
}
