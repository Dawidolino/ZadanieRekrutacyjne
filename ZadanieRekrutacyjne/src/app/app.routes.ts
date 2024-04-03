import { Routes } from '@angular/router';
import { TagListComponent } from './tag-list/tag-list.component';

export const routes: Routes = [{ path: '', component: TagListComponent }, // Default route for tag list
{ path: 'tags', component: TagListComponent }, // Route for explicit tag list access
];
