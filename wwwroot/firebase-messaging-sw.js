self.skipWaiting();
self.addEventListener('activate', event => {
    event.waitUntil(clients.claim());
});

importScripts('https://www.gstatic.com/firebasejs/12.10.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/12.10.0/firebase-messaging-compat.js');

firebase.initializeApp({
    apiKey: "AIzaSyCpJJa-4S4tE5JaXlTQDweQq34RproD3v0",
    authDomain: "albait-55f36.firebaseapp.com",
    projectId: "albait-55f36",
    storageBucket: "albait-55f36.firebasestorage.app",
    messagingSenderId: "730731610581",
    appId: "1:730731610581:web:eb105232ed1fbe1b53fd5c",
    measurementId: "G-VDYDG46Z03"
});

const messaging = firebase.messaging();

self.addEventListener('push', function (event) {
    console.log('Push event received:', event);
    const data = event.data ? event.data.json() : {};
    console.log('Push data:', data);

    const title = data.notification?.title || data.data?.title || 'Notification';
    const body = data.notification?.body || data.data?.body || '';

    event.waitUntil(
        self.registration.showNotification(title, {
            body: body,
            icon: '/icon.png'
        })
    );
});

messaging.onBackgroundMessage(function (payload) {
    console.log('Background message received:', payload);
});