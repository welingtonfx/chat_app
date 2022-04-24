docker container rm stock-bot
docker image build -t chat-app/stock-bot .
docker container run -d --name stock-bot --network=wfx chat-app/stock-bot