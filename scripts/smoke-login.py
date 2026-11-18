"""Solo CI: prueba HTTPS, antiforgery, cookies y lectura de catálogos reales."""
import http.cookiejar
import os
import ssl
import urllib.parse
import urllib.request
from html.parser import HTMLParser


class HiddenInputs(HTMLParser):
    def __init__(self):
        super().__init__()
        self.fields = {}

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == 'input' and attrs.get('type') == 'hidden' and attrs.get('name'):
            self.fields[attrs['name']] = attrs.get('value', '')


# El certificado local de Caddy es autofirmado y solo se usa en este runner.
client = urllib.request.build_opener(
    urllib.request.HTTPSHandler(context=ssl._create_unverified_context()),
    urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
base = 'https://localhost'
with client.open(base + '/Account/Login', timeout=30) as response:
    parser = HiddenInputs()
    parser.feed(response.read().decode())
assert '__RequestVerificationToken' in parser.fields
parser.fields.update({'Input.Email': os.environ['BootstrapAdmin__Email'],
                      'Input.Password': os.environ['BootstrapAdmin__Password']})
with client.open(base + '/Account/Login',
                 data=urllib.parse.urlencode(parser.fields).encode(), timeout=30) as response:
    assert urllib.parse.urlparse(response.url).path == '/admin', 'Login no completado'
for path in ['/admin/catalogos/tez', '/admin/catalogos/regiones-corporales']:
    with client.open(base + path, timeout=30) as response:
        assert urllib.parse.urlparse(response.url).path == path
        html = response.read().decode()
        assert 'Falta configurar' not in html and 'No se pudo' not in html
print('HTTPS, login, cookie y catálogos: OK')
