import io
import unittest
from http.server import BaseHTTPRequestHandler, HTTPServer
from threading import Thread

import joblib

from app.artifact_service import upload_artifact
from app.settings import ExecutorSettings


class ArtifactUploadHandler(BaseHTTPRequestHandler):
    uploaded_bytes: bytes = b""

    def do_PUT(self) -> None:  # noqa: N802
        content_length = int(self.headers.get("Content-Length", "0"))
        ArtifactUploadHandler.uploaded_bytes = self.rfile.read(content_length)
        self.send_response(204)
        self.end_headers()

    def log_message(self, format: str, *args) -> None:  # noqa: A003
        return None


class ArtifactServiceTests(unittest.TestCase):
    def test_should_upload_serialized_artifact_bundle(self) -> None:
        server = HTTPServer(("127.0.0.1", 0), ArtifactUploadHandler)
        thread = Thread(target=server.serve_forever)
        thread.start()

        try:
            upload_artifact(
                ExecutorSettings(shared_secret="test-secret", http_timeout_seconds=5),
                f"http://127.0.0.1:{server.server_port}/artifacts",
                {"model": {"name": "demo"}},
            )

            self.assertGreater(len(ArtifactUploadHandler.uploaded_bytes), 0)
            deserialized_payload = joblib.load(io.BytesIO(ArtifactUploadHandler.uploaded_bytes))
            self.assertEqual(deserialized_payload["model"]["name"], "demo")
        finally:
            server.shutdown()
            server.server_close()
            thread.join()
