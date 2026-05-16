import { HttpInterceptorFn, HttpRequest, HttpResponse } from '@angular/common/http';
import { tap } from 'rxjs/operators';

const etagCache = new Map<string, string>();

function cacheKey(req: HttpRequest<unknown>): string {
  return `${req.method.toUpperCase()} ${stripQuery(req.url)}`;
}

function stripQuery(url: string): string {
  const i = url.indexOf('?');
  return i >= 0 ? url.slice(0, i) : url;
}

function getKeyForMutation(req: HttpRequest<unknown>): string | null {
  const url = stripQuery(req.url);
  const method = req.method.toUpperCase();

  if (method === 'PUT' || method === 'DELETE') {
    return `GET ${url}`;
  }

  if (method === 'POST' && url.endsWith('/send')) {
    const resource = url.replace(/\/send$/, '');
    return `GET ${resource}`;
  }

  return null;
}

export const etagInterceptor: HttpInterceptorFn = (req, next) => {
  const mutationKey = getKeyForMutation(req);
  let outgoing = req;

  if (mutationKey) {
    const etag = etagCache.get(mutationKey);
    if (etag) {
      outgoing = req.clone({ setHeaders: { 'If-Match': etag } });
    }
  }

  return next(outgoing).pipe(
    tap(event => {
      if (event instanceof HttpResponse) {
        const etag = event.headers.get('ETag') ?? event.headers.get('etag');
        if (etag && req.method.toUpperCase() === 'GET') {
          etagCache.set(cacheKey(req), etag);
        } else if (etag && (req.method.toUpperCase() === 'PUT' || req.method.toUpperCase() === 'POST')) {
          const url = stripQuery(req.url).replace(/\/send$/, '');
          etagCache.set(`GET ${url}`, etag);
        }
      }
    })
  );
};
