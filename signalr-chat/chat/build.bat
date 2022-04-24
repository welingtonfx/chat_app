docker rm -f signal-r
docker image build -t chat-app/signal-r .
docker container run -d --name signal-r --network=wfx -p 7240:80 -p 7241:7240 chat-app/signal-r