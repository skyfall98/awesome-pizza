import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { GetPizzaDTO } from '@dtos/pizza/get-pizza-dto';
import { environment } from '@environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PizzaService {
  private readonly _http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrls.backend + '/pizza';

  getAllPizzas(): Observable<GetPizzaDTO[]> {
    return this._http.get<GetPizzaDTO[]>(this.apiUrl);
  }
}
