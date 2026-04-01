import json
import unittest
from http.server import BaseHTTPRequestHandler, HTTPServer
from threading import Thread

from app.dataset_loader import load_dataset


class DatasetLoaderTests(unittest.TestCase):
    def test_should_load_json_rows_payload(self) -> None:
        payload = json.dumps({"rows": [{"age": 30}, {"age": 31}]})

        class Handler(BaseHTTPRequestHandler):
            def do_GET(self) -> None:  # noqa: N802
                encoded_payload = payload.encode("utf-8")
                self.send_response(200)
                self.send_header("Content-Type", "application/json")
                self.send_header("Content-Length", str(len(encoded_payload)))
                self.end_headers()
                self.wfile.write(encoded_payload)

            def log_message(self, format: str, *args) -> None:  # noqa: A003
                return None

        server = HTTPServer(("127.0.0.1", 0), Handler)
        thread = Thread(target=server.serve_forever)
        thread.start()

        try:
            dataframe = load_dataset(f"http://127.0.0.1:{server.server_port}/dataset", "json", 5)

            self.assertEqual(list(dataframe.columns), ["age"])
            self.assertEqual(len(dataframe.index), 2)
        finally:
            server.shutdown()
            server.server_close()
            thread.join()


if __name__ == "__main__":
    unittest.main()
