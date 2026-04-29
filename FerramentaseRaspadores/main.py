from flask import Flask, render_template

app = Flask(__name__)

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/ferramentas')
def ferramentas():
    return render_template('ferramentas.html')

@app.route('/raspadores')
def raspadores():   
    return render_template('raspadores.html')

if __name__ == '__main__':
    app.run(debug=True)