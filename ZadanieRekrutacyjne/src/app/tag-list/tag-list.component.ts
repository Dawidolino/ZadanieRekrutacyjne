import { Component, OnInit } from '@angular/core';
import { TagService } from '../services/tag.service';
import { Tag } from '../models/tag';

@Component({
  selector: 'app-tag-list',
  templateUrl: './tag-list.component.html',
  styleUrls: ['./tag-list.component.css']
})
export class TagListComponent implements OnInit {
  tags: Tag[] = [];
  totalTags: number = 100; // Specify the total number of tags to fetch
  currentPage: number = 1;
  sortBy: string = 'name';
  sortOrder: string = 'asc';

  constructor(private tagService: TagService) { }

  ngOnInit(): void {
    this.loadTags();
  }

  loadTags() {
    this.tagService.getTags(this.totalTags, this.currentPage, this.sortBy, this.sortOrder)
      .subscribe(tags => this.tags = tags);
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadTags();
  }

  onSortChange(event: any) {
    const sortBy = event.target.value;
    const sortOrder = event.target.selectedIndex === 0 ? 'asc' : 'desc'; // Assuming the first option is ascending
    this.loadTags();
  }
  
}
