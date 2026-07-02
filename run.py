from app import create_app, db
from app.models.user import User, Role
from app.models.book import Book

app = create_app()

@app.shell_context_processor
def make_shell_context():
    return {'db': db, 'User': User, 'Book': Book}

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8080)
