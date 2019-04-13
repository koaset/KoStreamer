# build environment
FROM node:9.6.1 as builder
RUN mkdir /usr/src/app
WORKDIR /usr/src/app
ENV PATH /usr/src/app/node_modules/.bin:$PATH
COPY package.json /usr/src/app/package.json
RUN npm install --silent
RUN npm install react-scripts@1.1.1 -g --silent
COPY ./src /usr/src/app/src
COPY ./public /usr/src/app/public

RUN npm run build

# production environment
FROM nginx:1.13.9-alpine
WORKDIR /usr/share/nginx/html/player
COPY --from=builder /usr/src/app/build .
COPY ./nginx.conf /
COPY ./docker-start.sh .

EXPOSE 80
ENTRYPOINT [ "./docker-start.sh" ]
CMD [ "nginx", "-g", "daemon off;" ]